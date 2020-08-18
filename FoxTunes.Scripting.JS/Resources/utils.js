function version() {
    //The actual version could be returned.
    //return Publication.Product + " " + Publication.Version;
    return "Fox Tunes";
}

function timestamp(value) {

    if (!value) {
        return value;
    }

    var s = parseInt((value / 1000) % 60);
    var m = parseInt((value / (1000 * 60)) % 60);
    var h = parseInt((value / (1000 * 60 * 60)) % 24);

    var parts = [];

    if (h > 0) {
        if (h < 10) {
            h = "0" + h;
        }
        parts.push(h);
    }

    if (m < 10) {
        m = "0" + m;
    }
    parts.push(m);
    if (s < 10) {
        s = "0" + s;
    }
    parts.push(s);

    return parts.join(":");
}

function zeropad(value, width) {
    value = value + "";
    return value.length >= width ? value : new Array(width - value.length + 1).join("0") + value;
}

function ucfirst(value) {
    if (!value) {
        return value;
    }
    var words = value.split(" ");
    for (var a = 0; a < words.length; a++) {
        words[a] = words[a].charAt(0).toUpperCase() + words[a].slice(1);
    }
    return words.join(" ");
}

function directoryname(value) {
    if (!value) {
        return value;
    }
    var parts = value.split('\\');
    if (parts.length > 1) {
        return parts[parts.length - 2];
    }
    else {
        return value;
    }
}

function filename(value) {
    if (!value) {
        return value;
    }
    var parts = value.split('\\');
    return parts[parts.length - 1].replace(/\.[^/.]+$/, '');
}

function extension(value) {
    if (!value) {
        return value;
    }
    {
        var parts = value.split("://");
        if (parts.length > 1) {
            return parts[0];
        }
    }
    {
        var parts = value.split(".");
        return parts[parts.length - 1];
    }
}