$(document).ready(() => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hub")
        .build();

    connection.on("ReceiveMessage", (user, message) => {
        const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        const encodedMsg = user + " says " + msg;
        const li = document.createElement("li");
        li.textContent = encodedMsg;
        document.getElementById("messagesList").appendChild(li);
    });

    connection.start().catch(err => console.error(err.toString()));

    Vue.filter('short', function (value) {
        if (!value) return ''
        value = value.toString()
        return value.slice(0, 10);
    });

    Vue.component('block', {
        template: '#block-template',
        props: ['block'],
    });

    const i18n = new VueI18n({ locale: 'zh', messages: i18ninfo, });

    var appdata = Object.assign({}, testdata, {
        languages: [{ display: '中', name: 'zh' }, { display: 'en', name: 'en' },],
        language: 'zh',
    });

    var app = new Vue({
        i18n: i18n,
        el: '#app',
        data: appdata,
        methods: {
            start: function () {
                connection.invoke("Start").catch(err => console.error(err.toString()));
            },
            stop: function () {
                connection.invoke("Stop").catch(err => console.error(err.toString()));
            },
            addNode: function () {
                connection.invoke("AddNode").catch(err => console.error(err.toString()));
            },
            changeLanguage: function (language) {
                i18n.locale = language;
                app.language = language;
            },
        },
    });

    connection.on("Update", (data) => {
        console.log("received", data);
        Object.assign(app, data);
    });

    $(window).on('hashchange', function () {
        let hash = window.location.hash;
        $('a.node-anchor').removeClass('active');
        $('a[href="' + hash + '"].node-anchor').addClass('active');
        console.log("hash", hash);
    });

    $(function () {
        $('[data-toggle="tooltip"]').tooltip()
    })
});

