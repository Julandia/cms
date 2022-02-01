import Vue from 'vue';
import axios from 'axios';
import App from './App.vue';
import router from './router';

// Import the Auth0 configuration
import {
  domain, clientId, audience, baseUrl,
} from '../auth_config.json';

// Import the plugin here
import { Auth0Plugin } from './auth/authWrapper';

Vue.config.productionTip = false;

// Reading from environment does not work on Azure. read from config file instead.
// const baseUrl = process.env.VUE_APP_API_URL;
const axiosConfig = {
  baseURL: baseUrl,
  timeout: 30000,
};

Vue.prototype.$axios = axios.create(axiosConfig);

// Install the authentication plugin here
Vue.use(Auth0Plugin, {
  domain,
  clientId,
  audience,
  onRedirectCallback: (appState) => {
    router.push(
      appState && appState.targetUrl
        ? appState.targetUrl
        : window.location.pathname,
    );
  },
});

new Vue({
  router,
  render: (h) => h(App),
}).$mount('#app');
