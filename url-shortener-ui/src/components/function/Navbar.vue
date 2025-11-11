<template>
  <nav class="navbar">
    <div class="navbar-inner">
      <router-link to="/" class="brand">Short URL Website</router-link>

      <div class="nav-links">
        <template v-if="isLoggedIn">
          <span class="hello-text">Hello, {{ user.username }}</span>

          <button class="menu-btn" @click="toggleMenu">
            <span></span>
            <span></span>
            <span></span>
          </button>

          <div v-if="isMenuOpen" class="menu-dropdown">
            <button @click="navigate('/settings')">Settings</button>
            <button @click="navigate('/history')">History</button>
            <button class="danger" @click="onLogout">Logout</button>
          </div>
        </template>

        <template v-else>
          <router-link to="/login">Login</router-link>
          <router-link to="/register">Register</router-link>
        </template>
      </div>
    </div>
  </nav>
</template>


<script setup>
defineOptions({ name: 'AppNavbar' })
import { ref } from 'vue'

defineProps({
  isLoggedIn: { type: Boolean, default: false },
  user: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['logout', 'navigate'])
const isMenuOpen = ref(false)

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

const onLogout = () => {
  emit('logout')
}

const navigate = (path) => {
  emit('navigate', path)
}
</script>
