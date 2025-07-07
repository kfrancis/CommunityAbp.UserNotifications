// sse-client.ts
export interface ISseClientOptions {
    endpoint?: string;
    reconnectInterval?: number;
    maxReconnectAttempts?: number;
    withCredentials?: boolean;
    onConnected?: (connectionId: string, data: any) => void;
    onDisconnected?: () => void;
    onReconnecting?: (attempt: number) => void;
    onReconnected?: (connectionId: string) => void;
    onHeartbeat?: (data: any) => void;
    onError?: (error: Event) => void;
}

export interface INotificationData {
    message: string;
    severity: "success" | "info" | "warning" | "error";
    title?: string;
    properties?: Record<string, any>;
}

export class SseClient {
    private eventSource: EventSource | null = null;
    private eventHandlers: Map<string, ((data: any) => void)[]> = new Map();
    private reconnectTimer: any = null;
    private reconnectAttempt = 0;
    private connectionId: string | null = null;
    private connected = false;
    private groups: string[] = [];

    constructor(
        private readonly options: ISseClientOptions = {}
    ) {
        this.options = {
            endpoint: "/api/notifications/sse/connect",
            reconnectInterval: 5000,
            maxReconnectAttempts: 10,
            withCredentials: true,
            ...options
        };

        // Register built-in event handlers
        this.on("connected", (data) => {
            this.connectionId = data.connectionId;
            this.connected = true;
            this.groups = data.groups || [];
            this.reconnectAttempt = 0;

            if (this.options.onConnected) {
                this.options.onConnected(data.connectionId, data);
            }
        });

        this.on("heartbeat", (data) => {
            if (this.options.onHeartbeat) {
                this.options.onHeartbeat(data);
            }
        });
    }

    /**
     * Connects to the SSE endpoint
     * @param group Optional group to join
     */
    connect(group?: string): Promise<string> {
        return new Promise((resolve, reject) => {
            this.disconnect();

            const endpoint = group
                ? `${this.options.endpoint}?groupName=${encodeURIComponent(group)}`
                : this.options.endpoint;

            this.eventSource = new EventSource(endpoint, {
                withCredentials: this.options.withCredentials
            });

            this.eventSource.onopen = () => {
                // Connection established, but waiting for the 'connected' event
                // to get the connectionId
            };

            // Setup event listeners for all registered event types
            this.eventHandlers.forEach((_, eventName) => {
                this.setupEventListener(eventName);
            });

            // Special handling for the 'connected' event to resolve the promise
            const connectedHandler = (event: MessageEvent) => {
                try {
                    const data = JSON.parse(event.data);
                    this.connectionId = data.connectionId;
                    this.connected = true;
                    resolve(data.connectionId);

                    // Remove this one-time handler
                    this.eventSource?.removeEventListener("connected", connectedHandler);
                } catch (error) {
                    reject(error);
                }
            };

            this.eventSource.addEventListener("connected", connectedHandler);

            this.eventSource.onerror = (error) => {
                if (this.options.onError) {
                    this.options.onError(error);
                }

                if (this.connected) {
                    // We were connected but lost connection
                    this.connected = false;

                    if (this.options.onDisconnected) {
                        this.options.onDisconnected();
                    }

                    this.reconnect();
                } else {
                    // Initial connection failed
                    reject(error);
                }
            };
        });
    }

    /**
     * Disconnects from the SSE endpoint
     */
    disconnect(): void {
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }

        if (this.reconnectTimer) {
            clearTimeout(this.reconnectTimer);
            this.reconnectTimer = null;
        }

        if (this.connected) {
            this.connected = false;

            if (this.options.onDisconnected) {
                this.options.onDisconnected();
            }
        }
    }

    /**
     * Registers an event handler
     * @param eventName Event name to listen for
     * @param callback Callback function to invoke when the event is received
     * @returns Unsubscribe function
     */
    on<T = any>(eventName: string, callback: (data: T) => void): () => void {
        if (!this.eventHandlers.has(eventName)) {
            this.eventHandlers.set(eventName, []);

            if (this.eventSource) {
                this.setupEventListener(eventName);
            }
        }

        const handlers = this.eventHandlers.get(eventName) as ((data: any) => void)[];
        handlers.push(callback);

        // Return unsubscribe function
        return () => {
            const index = handlers.indexOf(callback);
            if (index !== -1) {
                handlers.splice(index, 1);
            }
        };
    }

    /**
     * Joins a group
     * @param groupName Name of the group to join
     */
    async joinGroup(groupName: string): Promise<void> {
        if (!this.connectionId || !this.connected) {
            throw new Error("Not connected");
        }

        const response = await fetch("/api/notifications/sse/group/join", {
            method: "POST",
            headers: {
                'Content-Type': "application/json"
            },
            body: JSON.stringify({
                connectionId: this.connectionId,
                groupName
            }),
            credentials: "include"
        });

        if (response.ok) {
            if (this.groups.indexOf(groupName) !== -1) {
                // Array contains the groupName
                this.groups.splice(this.groups.indexOf(groupName), 1);
            }
        } else {
            throw new Error(`Failed to join group: ${response.statusText}`);
        }
    }

    /**
     * Leaves a group
     * @param groupName Name of the group to leave
     */
    async leaveGroup(groupName: string): Promise<void> {
        if (!this.connectionId || !this.connected) {
            throw new Error("Not connected");
        }

        const response = await fetch("/api/notifications/sse/group/leave", {
            method: "POST",
            headers: {
                'Content-Type': "application/json"
            },
            body: JSON.stringify({
                connectionId: this.connectionId,
                groupName
            }),
            credentials: "include"
        });

        if (response.ok) {
            const index = this.groups.indexOf(groupName);
            if (index !== -1) {
                this.groups.splice(index, 1);
            }
        } else {
            throw new Error(`Failed to leave group: ${response.statusText}`);
        }
    }

    /**
     * Gets the current connection ID
     */
    getConnectionId(): string | null {
        return this.connectionId;
    }

    /**
     * Gets the groups this connection is a member of
     */
    getGroups(): string[] {
        return [...this.groups];
    }

    /**
     * Checks if the connection is active
     */
    isConnected(): boolean {
        return this.connected;
    }

    private setupEventListener(eventName: string): void {
        if (!this.eventSource) return;

        this.eventSource.addEventListener(eventName, (event: MessageEvent) => {
            try {
                const data = JSON.parse(event.data);
                const handlers = this.eventHandlers.get(eventName);

                if (handlers) {
                    handlers.forEach(callback => {
                        try {
                            callback(data);
                        } catch (error) {
                            console.error(`Error in SSE event handler for '${eventName}':`, error);
                        }
                    });
                }
            } catch (error) {
                console.error(`Error parsing SSE event data for '${eventName}':`, error);
            }
        });
    }

    private reconnect(): void {
        if (this.reconnectTimer) {
            clearTimeout(this.reconnectTimer);
        }

        this.reconnectAttempt++;

        if (this.options.maxReconnectAttempts && this.reconnectAttempt > this.options.maxReconnectAttempts) {
            console.error("Max reconnect attempts reached");
            return;
        }

        if (this.options.onReconnecting) {
            this.options.onReconnecting(this.reconnectAttempt);
        }

        this.reconnectTimer = setTimeout(() => {
            console.log(`Attempting to reconnect (${this.reconnectAttempt})...`);

            this.connect()
                .then(connectionId => {
                    if (this.options.onReconnected) {
                        this.options.onReconnected(connectionId);
                    }
                })
                .catch(() => {
                    this.reconnect();
                });
        }, this.options.reconnectInterval);
    }
}

// Automatically register as an ABP module
if (typeof window !== "undefined" && window.abp) {
    window.abp.sseNotifications = {
        createClient: (options?: ISseClientOptions) => new SseClient(options)
    };
}