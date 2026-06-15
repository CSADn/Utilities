const path = require('path');

module.exports = {
  entry: {
    bundle: './src/index.ts',
    'player.bundle': './src/player/PlayerSetup.ts',
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'dist'),
    clean: true,
  },
  resolve: {
    extensions: ['.ts', '.js'],
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  target: 'web',
  optimization: {
    minimize: false,
  },
};
