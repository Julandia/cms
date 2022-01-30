import Vue from 'vue';
import VueRouter from 'vue-router';
import Home from '../views/Home.vue';
import Profile from '../views/Profile.vue';
import authGuard from '../auth/authGuard';
import ExternalApiView from '../views/ExternalApi.vue';
import Conventions from '../views/Conventions.vue';
import MyConventions from '../views/MyConventions.vue';

Vue.use(VueRouter);

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home,
  },
  {
    path: '/about',
    name: 'About',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: "about" */ '../views/About.vue'),
  },
  {
    path: '/profile',
    name: 'Profile',
    component: Profile,
    beforeEnter(to, from, next) {
      authGuard.authGuard(to, from, next);
    },
  },
  {
    path: '/external-api',
    name: 'external-api',
    component: ExternalApiView,
    // beforeEnter: authGuard,
  },
  {
    path: '/conventions',
    name: 'Conventions',
    component: Conventions,
    // beforeEnter: authGuard,
  },
  {
    path: '/myconventions',
    name: 'MyConventions',
    component: MyConventions,
    // beforeEnter: authGuard,
  },
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes,
});

export default router;
