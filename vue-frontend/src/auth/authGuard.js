import Vue from 'vue';

export default {
  authGuard: (to, from, next) => {
    const authService = Vue.prototype.$auth;
    // if (authService === null || authService.auth0Client === null) {
    //   Vue.prototype.$auth = useAuth0();
    //   authService = getInstance();
    // }

    const fn = () => {
      // If the user is authenticated, continue with the route
      if (authService.isAuthenticated) {
        return next();
      }

      // Otherwise, log in
      authService.loginWithRedirect({ appState: { targetUrl: to.fullPath } });
      return next(false);
    };

    // If loading has already finished, check our auth state using `fn()`
    if (!authService.loading) {
      return fn();
    }

    // Watch for the loading property to change before we check isAuthenticated
    // eslint-disable-next-line consistent-return
    authService.$watch('loading', (loading) => {
      if (loading === false) {
        return fn();
      }
    });

    return next();
  },
};
