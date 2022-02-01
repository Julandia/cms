<template>
  <div>
    <button @click="callPing">Ping</button>
    <button @click="callEcho">Echo</button>
    <p>{{ apiMessage }}</p>
  </div>
</template>

<script>
export default {
  name: 'external-api',
  data() {
    return {
      apiMessage: 'Empty',
    };
  },
  methods: {
    async callPing() {
      // Use Axios to make a call to the API
      const { data } = await this.$axios.get('/healthy/ping');

      this.apiMessage = data;
    },
    async callEcho() {
      // Get the access token from the auth wrapper
      const token = await this.$auth.getTokenSilently();
      const message = 'Hello';
      // Use Axios to make a call to the API
      const { data } = await this.$axios.post(`/healthy/echo?message=${message}`, '', {
        headers: {
          Authorization: `Bearer ${token}`, // send the access token through the 'Authorization' header
        },
      });

      this.apiMessage = data;
    },
  },
};
</script>
