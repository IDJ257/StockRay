export function connect() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/sym-notif")
        .withAutomaticReconnect()
        .build();

    const start = async () => {
        await connection.start();

    };

    const joinPublicGroup = async (onUpdate) => {

        await start();

        connection.on("ReceivePublicUpdate", (updates) => {
            if (!Array.isArray(updates)) return;
            onUpdate(updates);
        })

        await connection.invoke("JoinGroup", "Public");
    };

    const joinGroups = async (groups, onUpdate) => {
        await start();

        connection.on("ReceiveGroupUpdate", (updates) => {
            if (!Array.isArray(updates)) return;
            onUpdate(updates);
        });

        if (Array.isArray(groups) && groups.length > 0) {
            await connection.invoke("JoinGroups", groups);
        }
    };

    const stop = async () => {
        await connection.stop();
    };

    return {
        joinPublicGroup,
        joinGroups
    };

}


