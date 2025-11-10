<script>
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'
import Navbar from '../function/Navbar.vue'

// Use a relative path so Vite dev server can proxy `/api` -> backend and avoid CORS.
// If you prefer calling the backend directly (no proxy), set this to the full URL.
const API_URL = 'http://localhost:5000'

export default {
  name: 'UserRegister',
  components: { Navbar },
  setup() {
    const username = ref('')
    const email = ref('')
    const password = ref('')
    const isLoading = ref(false)
    const errorMessage = ref('')
    const successMessage = ref('')
    const router = useRouter()

    const register = async () => {
      errorMessage.value = ''
      successMessage.value = ''
      isLoading.value = true

      try {
        await axios.post(`${API_URL}/api/auth/register`, {
          username: username.value,
          email: email.value,
          password: password.value,
        })

        successMessage.value = 'Registration successful! Redirecting...'
        setTimeout(() => router.push('/login'), 1500)
      } catch (err) {
        errorMessage.value = err.response?.data || 'Registration failed. Please try again.'
      } finally {
        isLoading.value = false
      }
    }

    return { username, email, password, isLoading, errorMessage, successMessage, register }
  },
}
</script>

<template>
  <div class="app-shell">
    <Navbar />

    <!-- Register Form -->
    <main class="container">
      <div class="card">
        <h1 class="text-2xl font-bold text-center mb-1">Register</h1>
        <p class="text-center text-muted mb-6">Create a free account</p>

        <form @submit.prevent="register">
          <div class="input-group">
            <input
              type="text"
              v-model="username"
              placeholder="Username"
              :disabled="isLoading"
              required
            />
          </div>

          <div class="input-group">
            <input
              type="email"
              v-model="email"
              placeholder="Email"
              :disabled="isLoading"
              required
            />
          </div>

          <div class="input-group">
            <input
              type="password"
              v-model="password"
              placeholder="Password( min 6 characters )"
              :disabled="isLoading"
              minlength="6"
              required
            />
          </div>

          <button type="submit" class="button w-full" :disabled="isLoading">
            {{ isLoading ? '...' : 'Register' }}
          </button>
        </form>

        <!-- Success / Error -->
        <div v-if="successMessage" class="result success">
          <p>{{ successMessage }}</p>
        </div>
        <div v-if="errorMessage" class="result error">
          <p>{{ errorMessage }}</p>
        </div>

        <p class="text-center mt-4 text-sm">
          Already have an account?
          <router-link to="/login" class="text-accent hover:underline">Login</router-link>
        </p>
      </div>
    </main>
  </div>
</template>
