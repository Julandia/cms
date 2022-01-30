module.exports = {
  devServer: {
    proxy: {
      '/': {
        target: 'https://cmsbackend.azurewebsites.net/',
      },
    },
  },
};
