var path = require("path");
var webpack = require("webpack");
var htmlWebpackPlugin = require('html-webpack-plugin');
var copyWebpackPlugin = require('copy-webpack-plugin');

var config = {
    indexHtmlTemplate: "./src/ui/index.html"
    , fsharpEntry: "./src/ui/ui.fsproj"
    , outputDir: "./src/ui/publish"
    , assetsDir: "./src/ui/public"
    , devServerPort: 8080
    , babel: {
        presets: [
            ["@babel/preset-env", {
                "modules": false
                , "useBuiltIns": "usage"
				, corejs: 3
            }]
        ]
    }
}

var isProduction = !process.argv.find(v => v.indexOf('webpack-dev-server') !== -1);
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

// HtmlWebpackPlugin automatically injects <script> or <link> tags for generated bundles.
var commonPlugins = [
    new htmlWebpackPlugin({
        filename: 'index.html'
        , template: resolve(config.indexHtmlTemplate)
    })
];

module.exports = {
    // TODO-NMB?...In development, bundle styles together with the code so they can also trigger hot reloads; in production, put them in a separate CSS file.
    entry: { app: [resolve(config.fsharpEntry)] }
    , output: {
        path: resolve(config.outputDir)
        , filename: isProduction ? '[name].[hash].js' : '[name].js'
    }
    , mode: isProduction ? "production" : "development"
    , devtool: isProduction ? "" : "eval-source-map"
    , optimization: {
        splitChunks: {
            cacheGroups: {
                commons: {
                    test: /node_modules/,
                    name: "vendors",
                    chunks: "all"
                }
            }
        }
    }
    , plugins: isProduction
        ? commonPlugins.concat([
            new copyWebpackPlugin([{ from: resolve(config.assetsDir) }])
        ])
        : commonPlugins.concat([
            new webpack.HotModuleReplacementPlugin()
            , new webpack.NamedModulesPlugin()
        ])
    , resolve: {
        symlinks: false
    }
    , devServer: {
        publicPath: "/"
        , contentBase: resolve(config.assetsDir)
        , port: config.devServerPort
        , hot: true
        , inline: true
    }
    , module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/
                , use: {
                    loader: "fable-loader"
                    , options: {
                        babel: config.babel
                        , define: isProduction ? [ "ADAPTIVE_NO_TYPE_TESTS" ] : [ "ADAPTIVE_NO_TYPE_TESTS", "DEBUG" ]
                    }
                }
            }
            , {
                test: /\.js$/
                , exclude: /node_modules/
                , use: {
                    loader: 'babel-loader'
                    , options: config.babel
                },
            }
            , {
                test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/
                , use: ["file-loader"]
            }
        ]
    }
};

function resolve(filePath) {
    return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}
