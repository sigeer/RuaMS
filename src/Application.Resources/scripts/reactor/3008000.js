/**
 * @author: Ronan
 * @reactor: Water Fountain
 * @map: 930000800 - Forest of Poison Haze - Outer Forest Exit
 * @func: Water Fountain
 */

function hit() {
    const eim = rm.getEventInstance();
    if (eim != null)
        eim.giveEventPlayersExp(52000, rm.getMapId());
}

function act() {} //do nothing