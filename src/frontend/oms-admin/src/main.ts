import { createApp } from 'vue'
import PrimeVue from 'primevue/config'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Password from 'primevue/password'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import Timeline from 'primevue/timeline'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, { unstyled: true })
app.component('Button', Button)
app.component('Card', Card)
app.component('Column', Column)
app.component('DataTable', DataTable)
app.component('Dropdown', Dropdown)
app.component('InputText', InputText)
app.component('Message', Message)
app.component('Password', Password)
app.component('ProgressSpinner', ProgressSpinner)
app.component('Tag', Tag)
app.component('Timeline', Timeline)

app.mount('#app')
