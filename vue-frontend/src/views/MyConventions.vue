<template>
  <div>
    <h2>Click button to show all the conventions registered by me</h2>
    <button @click="callGetConventions">Show</button>
    <p>{{this.message}}</p>
    <ul>
        <li v-for="(convention, index) in conventions" :key="index">
          <p>{{convention.title}}</p>
          <ul>
             <li v-for="(event, evindex) in convention.events" :key="evindex">
               <p>{{event.title}}</p>
             </li>
          </ul>
        </li>
    </ul>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'myconventions',
  data() {
    return {
      conventions: [],
      message: '',
    };
  },
  methods: {
    async callGetConventions() {
      const token = await this.$auth.getTokenSilently();
      const userId = this.$auth.user.email;
      // Use Axios to make a call to the API
      const { data } = await axios.get(`/api/conventions/registered/${userId}`, {
        headers: {
          Authorization: `Bearer ${token}`, // send the access token through the 'Authorization' header
        },
      });
      console.log(data);
      this.conventions = data;
      if (data.length === 0) {
        this.message = 'You do not have any registered conventions.';
      } else {
        this.message = '';
      }
    },
  },
};
</script>
