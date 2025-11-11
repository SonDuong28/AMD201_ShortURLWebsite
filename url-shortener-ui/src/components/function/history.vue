<script setup>
defineOptions({ name: 'HistoryView' })
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'

const API_URL = 'http://localhost:5000'

const router = useRouter()

const historyItems = ref([])
const loading = ref(true)
const errorMessage = ref('')
const shortHost = API_URL

const isClearing = ref(false)
const deletingSelected = ref(false)

const selectedIds = ref([])

const loadHistory = async () => {
  const apiKey = localStorage.getItem('apiKey')

  if (!apiKey) {
    errorMessage.value = 'Please log in to view your shortened URL history.'
    loading.value = false
    return
  }

  try {
    const res = await axios.get(`${API_URL}/api/url/history`, {
      headers: { 'X-API-Key': apiKey },
    })
    historyItems.value = res.data
  } catch (err) {
    console.error('History error:', err)
    errorMessage.value = 'Unable to load history. Please try again later.'
  } finally {
    loading.value = false
  }
}

const isSelected = (id) => selectedIds.value.includes(id)

const toggleSelect = (id) => {
  if (isSelected(id)) {
    selectedIds.value = selectedIds.value.filter((x) => x !== id)
  } else {
    selectedIds.value.push(id)
  }
}

const toggleSelectAll = () => {
  if (!historyItems.value.length) return

  const allIds = historyItems.value.map((x) => x.id)
  const allSelected = allIds.every((id) => selectedIds.value.includes(id))

  selectedIds.value = allSelected ? [] : allIds
}

// Delete item selected
const deleteSelected = async () => {
  const apiKey = localStorage.getItem('apiKey')
  if (!apiKey) {
    errorMessage.value = 'Please log in again.'
    return
  }

  if (!selectedIds.value.length) return
  if (!confirm(`Delete ${selectedIds.value.length} selected item(s)?`)) return

  deletingSelected.value = true
  try {
    await Promise.all(
      selectedIds.value.map((id) =>
        axios
          .delete(`${API_URL}/api/url/history/${id}`, {
            headers: { 'X-API-Key': apiKey },
          })
          .catch((err) => {
            console.error('Delete selected error for id', id, err)
          }),
      ),
    )

    historyItems.value = historyItems.value.filter((x) => !selectedIds.value.includes(x.id))
    selectedIds.value = []
  } finally {
    deletingSelected.value = false
  }
}

// Delete all history
const clearAll = async () => {
  const apiKey = localStorage.getItem('apiKey')
  if (!apiKey) {
    errorMessage.value = 'Please log in again.'
    return
  }

  if (!historyItems.value.length) return
  if (!confirm('Delete ALL shortened URLs in your history?')) return

  isClearing.value = true
  try {
    await axios.delete(`${API_URL}/api/url/history`, {
      headers: { 'X-API-Key': apiKey },
    })
    historyItems.value = []
    selectedIds.value = []
  } catch (err) {
    console.error('Clear error:', err)
    alert(err.response?.data?.error || 'Failed to clear history. Please try again.')
  } finally {
    isClearing.value = false
  }
}

onMounted(loadHistory)
</script>

<template>
  <div class="app-shell">
    <main class="container">
      <div class="card history-card">
        <h1 class="text-2xl font-bold mb-3">Shortened URL History</h1>
        <p class="text-sm text-muted mb-4">List of links you have shortened while logged in.</p>

        <!-- Loading -->
        <div v-if="loading">Loading...</div>

        <!-- Error -->
        <div v-else-if="errorMessage" class="result error">
          {{ errorMessage }}
        </div>

        <!-- Empty -->
        <div v-else-if="!historyItems.length">
          You have no shortened URLs under this account yet.
        </div>

        <!-- Has data -->
        <div v-else>
          <table class="history-table">
            <thead>
              <tr>
                <th class="select-col">
                  <input
                    type="checkbox"
                    :checked="historyItems.length && selectedIds.length === historyItems.length"
                    @change="toggleSelectAll"
                  />
                </th>
                <th>Original URL</th>
                <th>Short URL</th>
                <th>Created At</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in historyItems" :key="item.id">
                <td class="select-col">
                  <input
                    type="checkbox"
                    :checked="isSelected(item.id)"
                    @change="toggleSelect(item.id)"
                  />
                </td>
                <td class="ellipsis" :title="item.originalUrl">
                  {{ item.originalUrl }}
                </td>
                <td>
                  <a :href="`${shortHost}/${item.shortCode}`" target="_blank">
                    {{ shortHost }}/{{ item.shortCode }}
                  </a>
                </td>
                <td>{{ new Date(item.createdAt).toLocaleString() }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="!loading" class="history-actions">
          <button class="button ghost" @click="router.push('/')">← Back to shorten page</button>

          <button
            class="button danger"
            :disabled="!selectedIds.length || deletingSelected"
            @click="deleteSelected"
          >
            {{
              deletingSelected
                ? 'Deleting selected...'
                : `Delete selected (${selectedIds.length || 0})`
            }}
          </button>

          <button
            class="button danger-outline"
            :disabled="isClearing || !historyItems.length"
            @click="clearAll"
          >
            {{ isClearing ? 'Clearing all...' : 'Clear all history' }}
          </button>
        </div>
      </div>
    </main>
  </div>
</template>
