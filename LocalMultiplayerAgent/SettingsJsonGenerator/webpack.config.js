const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
	entry: "./src/index.ts",
	mode: "production",
	plugins: [new MiniCssExtractPlugin()],
	module: {
		rules: [
			{
				test: /\.tsx?$/,
				use: "ts-loader",
				exclude: /node-modules/,
			},
			{
				test: /\.css$/i,
				use: [MiniCssExtractPlugin.loader, "css-loader"],
			},
		],
	},
	resolve: {
		extensions: [".ts", ".js", ".css"],
	},
	output: {
		filename: "index.js",
		path: path.resolve(__dirname),
	},
};
