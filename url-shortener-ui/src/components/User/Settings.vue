<script setup>
// Multi-word component name for ESLint
defineOptions({ name: 'UserSettings' })
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'
import Navbar from '../function/Navbar.vue'

const API_URL = 'http://localhost:5000'

const router = useRouter()

const username = ref('')
const email = ref('')
const newPassword = ref('')

const isLoading = ref(false)
const message = ref('')
const errorMessage = ref('')

const isLoggedIn = ref(false)
const user = ref({})

onMounted(() => {
  const apiKey = localStorage.getItem('apiKey')
  isLoggedIn.value = !!apiKey

  const storedUser = localStorage.getItem('user')
  if (storedUser) {
    const u = JSON.parse(storedUser)
    username.value = u.username || ''
    email.value = u.email || ''
    user.value = u
  }
})

const save = async () => {
  message.value = ''
  errorMessage.value = ''
  isLoading.value = true

  try {
    const apiKey = localStorage.getItem('apiKey')
    if (!apiKey) {
      errorMessage.value = 'Please log in again.'
      return
    }

    await axios.put(
      `${API_URL}/api/account`,
      {
        username: username.value,
        email: email.value,
        newPassword: newPassword.value || null,
      },
      {
        headers: { 'X-API-Key': apiKey },
      },
    )

    const updated = {
      username: username.value,
      email: email.value,
      apiKey,
    }
    localStorage.setItem('user', JSON.stringify(updated))

    message.value = 'Account updated successfully.'
    newPassword.value = ''
  } catch (err) {
    console.error(err)
    errorMessage.value = err.response?.data || 'Failed to update account. Please try again.'
  } finally {
    isLoading.value = false
  }
}

const cancel = () => {
  router.push('/')
}

const logout = () => {
  localStorage.removeItem('apiKey')
  localStorage.removeItem('user')
  isLoggedIn.value = false
  user.value = {}
  router.push('/')
}

const navigateTo = (path) => {
  router.push(path)
}
</script>

<template>
  <div class="app-shell">
    <Navbar :isLoggedIn="isLoggedIn" :user="user" @logout="logout" @navigate="navigateTo" />
    <main class="container">
      <div class="card settings-card">
        <h1>Your account</h1>
        <p class="subtitle">Manage your profile information and password.</p>

        <form @submit.prevent="save" class="settings-form">
          <div class="input-group vertical">
            <label>Username</label>
            <input type="text" v-model="username" required />
          </div>

          <div class="input-group vertical">
            <label>Email</label>
            <input class="email" type="email" v-model="email" required disabled title="Email cannot be changed" />
          </div>

          <div class="input-group vertical">
            <label>
              New password
              <span class="hint">(leave blank to keep current)</span>
            </label>
            <input type="password" v-model="newPassword" minlength="6" placeholder="••••••••" />
          </div>

          <div class="settings-actions">
            <button type="submit" class="button" :disabled="isLoading">
              {{ isLoading ? 'Saving...' : 'Save' }}
            </button>

            <button type="button" class="button ghost" @click="cancel">Cancel</button>
          </div>
        </form>

        <div v-if="message" class="result success">
          {{ message }}
        </div>
        <div v-if="errorMessage" class="result error">
          {{ errorMessage }}
        </div>
      </div>
    </main>
  </div>
</template>
