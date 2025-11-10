<script>
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'
import Navbar from '../function/Navbar.vue'

const API_URL = 'http://localhost:5000'

export default {
  name: 'UserLogin',
  components: { Navbar },
  setup() {
    const username = ref('')
    const password = ref('')
    const isLoading = ref(false)
    const errorMessage = ref('')
    const router = useRouter()

    const login = async () => {
      errorMessage.value = ''
      isLoading.value = true

      try {
        const res = await axios.post(`${API_URL}/api/auth/login`, {
          username: username.value,
          password: password.value
        })

        localStorage.setItem('apiKey', res.data.apiKey)
        localStorage.setItem('user', JSON.stringify(res.data))

        // Redirect to shorten page
        router.push('/')
      } catch (err) {
        errorMessage.value = err.response?.data || 'Invalid username or password.'
      } finally {
        isLoading.value = false
      }
    }

    return { username, password, isLoading, errorMessage, login }
  }
}
</script>

<template>

  <div class="app-shell">
    <Navbar />
    <main class="container">
      <div class="card">
        <h1 class="text-2xl font-bold text-center mb-1">Login</h1>
        <p class="text-center text-muted mb-6">Enter your information to continue</p>

        <form @submit.prevent="login">
          <div class="input-group">
            <input type="text" v-model="username" placeholder="Username" required />
          </div>
          <div class="input-group">
            <input type="password" v-model="password" placeholder="Password" required />
          </div>
          <button type="submit" class="button w-full" :disabled="isLoading">
            {{ isLoading ? '...' : 'Login' }}
          </button>
        </form>

        <div v-if="errorMessage" class="result error">
          <p>{{ errorMessage }}</p>
        </div>

        <p class="text-center mt-4 text-sm">
          Don't have an account? <router-link to="/register" class="text-accent hover:underline">Register</router-link>
        </p>
      </div>
    </main>
  </div>
</template>