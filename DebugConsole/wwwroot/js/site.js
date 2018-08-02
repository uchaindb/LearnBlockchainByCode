$(document).ready(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.on("ReceiveMessage", (user, message) => {
        const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        const encodedMsg = user + " says " + msg;
        const li = document.createElement("li");
        li.textContent = encodedMsg;
        document.getElementById("messagesList").appendChild(li);
    });

    connection.start().catch(err => console.error(err.toString()));

    document.getElementById("sendButton").addEventListener("click", event => {
        const user = document.getElementById("userInput").value;
        const message = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", user, message).catch(err => console.error(err.toString()));
        event.preventDefault();
    });

    var v = {
        author: "hh",
        status: [
            {
                name: "1",
                blocks: [
                    {
                        height: 2,
                        hash: "abcdefghijklmnopqrstuvabcdefghijklmnopqrstuv",
                    },]
            },
            {
                name: "2",
                blocks: [
                    {
                        height: 2,
                        hash: "abcdefghijklmnopqrstuvabcdefghijklmnopqrstuv",
                    },
                    {
                        height: 3,
                        hash: "abcdefghijklmnopqrstuvabcdefghijklmnopqrstuv",
                    },
                ]
            },
        ]
    };
    var html =$.templates("#statusTemplate").render(v);
    $(html).appendTo("#template-container");
});

