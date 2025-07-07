// abp.d.ts
interface Window {
    abp: {
        sseNotifications?: {
            createClient: (options?: any) => any;
        };
        // Add other ABP properties as needed
    };
}