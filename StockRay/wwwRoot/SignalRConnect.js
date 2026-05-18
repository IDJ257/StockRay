export function connect() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/sym-notif")
        .withAutomaticReconnect()
        .build();

    const start = async () => {
       
        await connection.start();

    };

    const joinPublicGroup = async () => {

        await connection.invoke("JoinGroup", "Public");
    };

    const joinGroups = async (groups) => {
  

        if (Array.isArray(groups) && groups.length > 0) {
            await connection.invoke("JoinGroups", groups);
        }
    };

    const stop = async () => {
        await connection.stop();
    };

    const leaveGroup = async (group) => {
        await connection.invoke("LeaveGroup", group)
    }

    return {
        connection,
        start,
        stop,
        joinPublicGroup,
        joinGroups,
        leaveGroup
    };

}


