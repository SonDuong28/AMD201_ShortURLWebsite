<script>
export default {
  setup() {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const logout = () => {
      localStorage.removeItem('apiKey')
      localStorage.removeItem('user')
      window.location.href = '/'
    }
    return { user, logout }
  }
}
</script>

<template>
  <div class="absolute top-4 right-4 flex items-center space-x-3 bg-white rounded-lg shadow p-2">
    <span class="text-sm font-medium">{{ user.username }}</span>
    <button @click="logout" class="text-xs text-red-600 hover:underline">
      Đăng xuất
    </button>
  </div>
</template>
