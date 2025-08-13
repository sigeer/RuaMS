function pay(meso, failMsg) {
    if (cm.getMeso() < meso) {
        cm.sendNextLevel(failMsg || "金币不足");
        return false;
    }
    else {
        cm.gainMeso(-meso);
        return true;
    }
}

Date.prototype.formatDate = function () {
    var year = this.getFullYear();
    var month = (this.getMonth() + 1).toString().padStart(2, '0');
    var day = (this.getDate()).toString().padStart(2, '0');
    var hour = (this.getHours()).toString().padStart(2, '0');
    var minute = (this.getMinutes()).toString().padStart(2, '0');
    var second = (this.getSeconds()).toString().padStart(2, '0');

    return `${year}-${month}-${day} ${hour}:${minute}:${second}`;
};

Date.prototype.formatTime = function () {
    var hour = (this.getHours()).toString().padStart(2, '0');
    var minute = (this.getMinutes()).toString().padStart(2, '0');
    var second = (this.getSeconds()).toString().padStart(2, '0');

    return `${hour}:${minute}:${second}`;
};