// module.exports = {
//   devServer: {
//     proxy: {
//       '/': {
//         target: 'https://cmsbackend.azurewebsites.net/',
//       },
//     },
//   },
// };
module.exports = {
  devServer: {
    disableHostCheck: true,
  },
};
