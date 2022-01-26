# LocalMultiplayerAgent MultiplayerSettings.json Generator

This folder contains a web page which generates the [MultiplayerSettings.json](../MultiplayerSettings.json) file based on your options.

Useful if you are having difficulty using `LocalMultiplayerAgent` or editing `MultiplayerSettings.json`.

![Screenshot of this JSON generator utility](./screenshot.png?raw=true)

## Instructions

1.  Clone this repository
2.  Open the `index.html` in this folder in any web browser
3.  Select options required by your server. The JSON on the right side will update immediately.
4.  Copy the JSON and paste into `MultiplayerSettings.json`

## Generating

To make changes to the JavaScript that runs this page:

1. Install Node JS
2. Run
    ```bash
    npm install
    npm run build
    ```
