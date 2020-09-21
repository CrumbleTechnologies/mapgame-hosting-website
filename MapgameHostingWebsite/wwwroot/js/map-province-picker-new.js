var imageToShow
var colouredImage

var chosenHexColour = "#000000"

var pixelIndexesChosen = []

function preload() {
    imageToShow = loadImage("res/epic-map.png")
    colouredImage = loadImage("res/edited map.png")
}

function setup() {
    var canvas = createCanvas(imageToShow.width, imageToShow.height)
    canvas.mouseClicked(imageClicked)
    canvas.parent("canvas-container")
    canvas.id("p5.js-canvas")


    document.getElementById("copy-button").addEventListener("click", () => {
        document.querySelector("#map-code-text-input").value = document.querySelector("#map-code-text-input").value.substring(0, document.querySelector("#map-code-text-input").value.length - 1)
        var a = document.createElement("a")
        document.body.appendChild(a)
        a.style = "display: none"
        var file = new Blob([document.querySelector("#map-code-text-input").value], { type: "text/plain" })
        a.href = window.URL.createObjectURL(file)
        a.download = "map_claim_code.txt" // name of file to download
        a.click()
        window.URL.revokeObjectURL(url)
        document.querySelector("#map-code-text-input").value += ","
    })
    document.getElementById("colour-input").addEventListener("change", (event) => {
        console.log(event.target.value)
        if (event.target.value == "#FFFFFF") {
            event.target.value = "#000000"
        }
        changeAllInstancesInCurrentMapCode(chosenHexColour, event.target.value)
        chosenHexColour = event.target.value
    }, false)

    document.getElementById("loading-container").style.visibility = "collapse"
    document.getElementById("main-container").style.visibility = "visible"
}

function draw() {
    updateShownImage()
    var img = image(imageToShow, 0, 0)
}

function updateShownImage() {
    imageToShow.loadPixels()

    pixelIndexesChosen.forEach(index => {
        var colourPicked = hexToRgb(chosenHexColour)
        imageToShow.pixels[index] = colourPicked.r
        imageToShow.pixels[index + 1] = colourPicked.g
        imageToShow.pixels[index + 2] = colourPicked.b
    });

    imageToShow.updatePixels()
}

function imageClicked() {
    var x = Math.floor(mouseX)
    var y = Math.floor(mouseY)
    pixelClicked = colouredImage.get(x, y) // pixelClicked = [r, g, b, a]
    clickColourData = pixelClicked

    imageToShow.loadPixels()
    colouredImage.loadPixels()

    fetch("res/colours with coords.json").then(response => response.json()).then(coloursWithCoordsJSON => {
        for (var i = 0; i < coloursWithCoordsJSON.main.length; i++) {
            if (coloursWithCoordsJSON.main[i][1][0] == clickColourData[0] && coloursWithCoordsJSON.main[i][1][1] == clickColourData[1] && coloursWithCoordsJSON.main[i][1][2] == clickColourData[2] && coloursWithCoordsJSON.main[i][1][3] == clickColourData[3]) {
                console.log(coloursWithCoordsJSON.main[i][0])
                var coordOfProvinceClicked = coloursWithCoordsJSON.main[i][0]

                var pixelToAddToCode = "none"
                for (let j = 0; j < colouredImage.pixels.length / 4; j++) {
                    if (colouredImage.pixels[j] == clickColourData[0]) {
                        if (colouredImage.pixels[j + 1] == clickColourData[1]) {
                            if (colouredImage.pixels[j + 2] == clickColourData[2]) {
                                if (pixelIndexesChosen.includes(j)) {
                                    imageToShow.pixels[j] = 255
                                    imageToShow.pixels[j + 1] = 255
                                    imageToShow.pixels[j + 2] = 255
                                    pixelIndexesChosen.splice(pixelIndexesChosen.indexOf(j), 1)
                                } else {
                                    pixelIndexesChosen.push(j)
                                }

                                if (pixelToAddToCode == "none") {
                                    pixelToAddToCode = coordOfProvinceClicked
                                }
                            }
                        }
                    }
                }

                imageToShow.updatePixels()
                if (pixelToAddToCode != "none") {
                    handleProvinceClick(pixelToAddToCode)
                }

                console.log("province updated")
                break
            }
        }
    })
}

function constructMapCode(pixel, hexColour) {
    return `${pixel[0]}.${pixel[1]}=${hexColour},`
}

function changeAllInstancesInCurrentMapCode(oldString, newString) {
    var mapCodeTextInput = document.getElementById("map-code-text-input")
    mapCodeTextInput.value = mapCodeTextInput.value.split(oldString).join(newString)
}

function handleProvinceClick(pixelToAddToCode) {
    if (pixelToAddToCode == undefined) {
        return
    }

    var mapClaimCode = constructMapCode(pixelToAddToCode, document.getElementById("colour-input").value)
    var mapCodeTextInput = document.getElementById("map-code-text-input")

    if (mapCodeTextInput.value.includes(mapClaimCode)) {
        mapCodeTextInput.value = mapCodeTextInput.value.replace(mapClaimCode, "")
    } else {
        mapCodeTextInput.value += mapClaimCode
    }
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}