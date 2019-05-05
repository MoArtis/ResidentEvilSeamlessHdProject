function discordAPI() {

    var init = {
        method: 'GET',
        mode: 'cors',
        cache: 'reload'
    }

    fetch('https://discordapp.com/api/guilds/549168005553192980/widget.json', init).then(function (response) {

        if (response.status != 200) {
            console.log("it didn't work" + response.status);
            document.getElementById("discord_widget_member").remove();
            return
        }
        response.json().then(function (data) {
            var users = data.members;
            if (users.length < 1) {
                document.getElementById("discord_widget_member").remove();
            }
            else {
                document.getElementById("discord_widget_member_count").innerHTML = users.length;
                if (users.length == 1) {
                    document.getElementById("discord_widget_member_plural").remove();
                }
            }
        })

    }).catch(function (err) {

        document.getElementById("discord_widget_member").remove();
        console.log('fetch error: ' + err)

    })
}

discordAPI();