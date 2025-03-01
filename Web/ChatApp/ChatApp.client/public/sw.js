self.addEventListener("install", event => {
    self.skipWaiting();
});

self.addEventListener("activate", event => {
    console.log("Service Worker activated");
});

self.addEventListener("message", event => {
    const { message } = event.data;
    if (message) {
        self.registration.showNotification(message.author.userName, {
            body: message.content,
            icon: `https://localhost:7147/api/Attachments/${message.author.imageId}`,
            data: {
                url: `${self.location.origin}/server/5abcd9ff-764a-44fc-be95-1713b215987d/${message.channelId}/${message.id}`
            },
        });
    }
});

self.addEventListener("notificationclick", event => {
    event.notification.close();
    const url = new URL(event.notification.data.url || "/", self.location.origin).href;

    event.waitUntil(
        clients.matchAll({ type: "window", includeUncontrolled: true }).then(windowClients => {
            for (const client of windowClients) {
                if (client.url === url) {
                    client.focus();
                    return;
                }
            }
            for (const client of windowClients) {
                if ("navigate" in client) {
                    return client.navigate(url)
                        .then(navigatedClient => navigatedClient.focus())
                        .catch(error => {
                            console.error('Navigation failed:', error);
                            return clients.openWindow(url);
                        });
                }
            }
            return clients.openWindow(url);
        })
    );
});