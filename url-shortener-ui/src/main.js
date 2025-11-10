import { createApp } from 'vue'
import App from './components/App.vue'
import router from './router'
import '../public/css/style.css'

createApp(App).use(router).mount('#app')
