export function connect() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/sym-notif")
        .withAutomaticReconnect()
        .build();

    const start = async (onUpdate) => {

        connection.on("ReceivePublicUpdate", (updates) => {
            if (!Array.isArray(updates)) return;

            onUpdate(updates);
        });

        await connection.start();
        await connection.invoke("JoinGroup", "Public");
    };

    const joinPublicGroup = async (group) => {
        await connection.invoke("JoinGroup", group);
    };

    const joinGroups = async (groups) => {
        if (!Array.isArray(groups) || groups.length === 0) return;

        await connection.invoke("JoinGroups", groups);
    };

    const stop = async () => {
        await connection.stop();
    };

    return {
        start,
        stop,
        joinPublicGroup,
        joinGroups
    };

}


