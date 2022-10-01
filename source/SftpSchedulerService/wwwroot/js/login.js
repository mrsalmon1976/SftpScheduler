const { createApp } = Vue

createApp({
    data() {
        return {
            message: 'Hello Vue!',
            isLoading: true
        }
    },
    methods: {
        async login() {
            this.isLoading = true;
            let payload = { userName: 'test', password: 'test' };

            let res = await axios.post('/api/login', payload);

            let data = res.data;
            console.log(data);
            if (res.status == 200) {
                window.location.href = '/dashboard';
            }
            else {
                this.isLoading = false;
            }
        }
    },
    mounted: function () {
        this.isLoading = false;
    }
}).mount('#app-login')