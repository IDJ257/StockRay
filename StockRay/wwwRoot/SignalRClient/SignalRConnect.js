
//Establishing connection builder
export function connect() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/sym-notif")
        .withAutomaticReconnect()
        .build();

        //connecting to the hub
    const start = async () => {
       
        await connection.start();

    };

    const joinPublicGroup = async () => {

        await connection.invoke("JoinGroup", "Public");
    };

    //Even if the user adds 1 group to the .NET service we always need to send an array even if it's [MSFT];
    const joinGroups = async (groups) => {
  

        if (Array.isArray(groups) && groups.length > 0) {
            await connection.invoke("JoinGroups", groups);
        }
    };

    const stop = async () => {
        await connection.stop();
    };

    //Here though we always pass single group not [];
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


