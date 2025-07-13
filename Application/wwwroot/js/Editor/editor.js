
class EfeuAction {
    constructor(element) {
        this.element = element;
        this.element.addEventListener("mousedown", #onMouseDown);
    }

    #onMouseDown(event) {
        console.log("kek!");
    }

    addInputNode(inputNode) {
        this.inputs.push(inputNode)
    }

    unlink() {
        this.element.removeEventListener("mousedown");
        this.element = null;
    }
}

class EfeuInputNode {
    constructor(element) {
        this.element = element;
        this.element.addEventListener("mousedown")
    }

    unlink() {

    }
}
