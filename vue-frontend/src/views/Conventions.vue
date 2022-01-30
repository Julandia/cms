<template>
  <div>
    <h2>Click button to show all the conventions</h2>
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
  name: 'conventions',
  data() {
    return {
      conventions: [],
      message: '',
    };
  },
  methods: {
    async callGetConventions() {
      // Use Axios to make a call to the API
      const { data } = await axios.get('/api/conventions');
      console.log(data);
      this.conventions = data;
      if (data.length === 0) {
        this.message = 'No conventions are available.';
      } else {
        this.message = '';
      }
    },
  },
};
</script>
