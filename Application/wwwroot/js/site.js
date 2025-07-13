
function quadratic_bezier(x1, y1, x2, y2) {
    let mx = x1 - ((x1 - x2) / 2)
    let my = y1 - ((y1 - y2) / 2)
    let cp1x = x1 - ((x1 - mx) / 2)
    let cp1y = y1
    let cp2x = x2 - ((x2 - mx) / 2)
    let cp2y = y2

    if (x2 - x1 < 60) {
        cp1x = x1 + 30
        cp1y = y1
        cp2x = x2 - 30
        cp2y = y2
    }

    return `M${x1},${y1} Q${cp1x},${cp1y} ${mx},${my} Q${cp2x},${cp2y} ${x2},${y2}`
}