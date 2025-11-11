import { createRouter, createWebHistory } from 'vue-router'

import Shorten from '../components/Shorten/Shorten.vue'
import Login from '../components/User/Login.vue'
import Register from '../components/User/Register.vue'
import Settings from '../components/function/Settings.vue'
import History from '../components/function/history.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Shorten,
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
  },
  {
    path: '/register',
    name: 'Register',
    component: Register,
  },
  {
    path: '/settings',
    name: 'Settings',
    component: Settings,
  },
  {
    path: '/history',
    name: 'History',
    component: History,
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
