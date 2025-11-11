<script setup>
defineOptions({ name: 'ShortenView' })
import { ref, onMounted } from 'vue'
import QRCode from 'qrcode'
import axios from 'axios'
import { useRouter, useRoute } from 'vue-router'
import Navbar from '../function/Navbar.vue'

// Use Vite env variable when available so built image can target a different API host
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

const router = useRouter()
const route = useRoute()

// State
const originalUrl = ref('')
const shortenedUrl = ref('')
const qrDataUrl = ref('')
const qrError = ref('')
const qrTarget = ref('')
const errorMessage = ref('')
const isLoading = ref(false)
const isLoggedIn = ref(false)
const user = ref({})
const isMenuOpen = ref(false)

// Initialization
onMounted(() => {
  const apiKey = localStorage.getItem('apiKey')
  const userData = localStorage.getItem('user')
  isLoggedIn.value = !!apiKey
  if (userData) {
    user.value = JSON.parse(userData)
  }
})

// ====== SHORTEN LINK FUNCTION ======
const shortenUrl = async () => {
  errorMessage.value = ''
  shortenedUrl.value = ''

  if (!originalUrl.value) {
    errorMessage.value = 'Please enter a URL.'
    return
  }

  isLoading.value = true

  try {
    const headers = {}
    const apiKey = localStorage.getItem('apiKey')
    if (apiKey) {
      headers['X-API-Key'] = apiKey
    }

    const res = await axios.post(`${API_URL}/api/url`, { url: originalUrl.value }, { headers })

    // Prefer the full shortUrl returned by backend; if missing, try to build from shortCode
    const fullShort =
      res.data?.shortUrl || (res.data?.shortCode ? `${API_URL}/${res.data.shortCode}` : '')

    shortenedUrl.value = fullShort

    // Generate QR for the ORIGINAL (long) URL that user entered, not the short URL.
    qrDataUrl.value = ''
    qrError.value = ''
    // Use the originalUrl input value. Normalize scheme if missing so QR opens correctly on phones.
    let longUrl = originalUrl.value || ''
    if (longUrl && !/^https?:\/\//i.test(longUrl)) {
      longUrl = 'https://' + longUrl
    }

    if (longUrl) {
      try {
        const dataUrl = await QRCode.toDataURL(String(longUrl), { width: 300 })
        qrDataUrl.value = dataUrl
        qrTarget.value = longUrl
      } catch (e) {
        console.error('QR generation error (original URL):', e)
        qrDataUrl.value = ''
        qrError.value = 'Unable to generate QR code for the original URL.'
      }
    } else {
      qrError.value = 'No original URL available to generate QR.'
    }
  } catch (err) {
    console.error('Shorten error:', err)
    errorMessage.value = err.response?.data || 'Connection error. Please try again.'
  } finally {
    isLoading.value = false
  }
}

// ====== LOGOUT ======
const logout = () => {
  localStorage.removeItem('apiKey')
  localStorage.removeItem('user')
  isLoggedIn.value = false
  user.value = {}
  isMenuOpen.value = false
  router.push('/')
}

// Note: hamburger/menu helpers were removed because they were not referenced in the template

const navigateTo = (path) => {
  router.push(path)
}
</script>

<template>
  <div class="app-shell">
    <!-- NAVBAR -->
    <Navbar :isLoggedIn="isLoggedIn" :user="user" @logout="logout" @navigate="navigateTo" />

    <!-- HOME PAGE – SHORTEN URL -->
    <main v-if="route.path === '/'" class="container">
      <div class="card">
        <h1 class="text-2xl font-bold text-center mb-1">Shorten URL</h1>
        <p class="text-center text-muted mb-6">Enter a link to create a short URL</p>

        <form @submit.prevent="shortenUrl">
          <div class="input-group">
            <input
              type="url"
              v-model="originalUrl"
              placeholder="https://example.com"
              :disabled="isLoading"
              required
            />
            <button type="submit" class="button" :disabled="isLoading">
              {{ isLoading ? '...' : 'Shorten' }}
            </button>
          </div>
        </form>

        <!-- Result -->
        <div v-if="shortenedUrl" class="result success">
          <p>Your short URL:</p>
          <a :href="shortenedUrl" target="_blank" class="font-medium break-all block">
            {{ shortenedUrl }}
          </a>
          <!-- QR code preview -->
          <div class="qr-block" style="margin-top: 12px">
            <p class="text-sm">QR code:</p>
            <template v-if="qrDataUrl">
              <img
                :src="qrDataUrl"
                alt="QR code"
                style="width: 180px; height: 180px; border-radius: 8px; margin-top: 8px"
              />

            </template>
            <template v-else>
              <p class="text-muted" style="margin-top: 8px">{{ qrError || 'Generating QR...' }}</p>
            </template>
          </div>
        </div>

        <!-- error -->
        <div v-if="errorMessage" class="result error">
          <p>{{ errorMessage }}</p>
        </div>

        <!-- Login suggestion -->
        <p v-if="!isLoggedIn" class="text-center mt-4 text-sm">
          <router-link to="/login" class="text-accent hover:underline">
            Sign in to save and view your shortened URLs.
          </router-link>
        </p>
      </div>
    </main>

    <!-- OTHER ROUTES -->
    <router-view v-if="route.path !== '/'" />
  </div>
</template>
