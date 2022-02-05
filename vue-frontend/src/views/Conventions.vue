<template>
  <div>
    <h2>Click button to show all the conventions</h2>
    <button @click="callGetConventions">Show</button>
    <ul>
        <li v-for="(convention, index) in conventions" :key="index">
          <div id="inlinearea">
            <p>{{convention.title}} ({{convention.totalNumberOfParticipants}} registered)
            <button v-if="$auth.isAuthenticated" @click="registerConvention(convention.id)">Register</button>
            </p>
          </div>
          <ul>
             <li v-for="(event, evindex) in convention.events" :key="evindex">
               <div id="inlinearea">
                {{event.title}} ({{event.totalNumberOfParticipants}} registered)
                <button v-if="$auth.isAuthenticated" @click="registerEvent(convention.id, event.id)">Register</button>
               </div>
             </li>
          </ul>
        </li>
    </ul>
  </div>
</template>

<script>
// import axios from 'axios';

export default {
  name: 'conventions',
  data() {
    return {
      conventions: [],
    };
  },
  methods: {
    async callGetConventions() {
      // Use Axios to make a call to the API
      const { data } = await this.$axios.get('/api/conventions');
      this.conventions = data;
      if (data.length === 0) {
        this.message = 'No conventions are available.';
      } else {
        this.message = '';
      }
    },
    async registerConvention(conventionId) {
      const token = await this.$auth.getTokenSilently();

      const registerInfo = {
        conventionId,
        userId: this.$auth.user.email,
        numberOfParticipants: 1,
      };
      // Use Axios to make a call to the API
      await this.$axios.post('/api/conventions/register/convention', registerInfo, {
        headers: {
          Authorization: `Bearer ${token}`, // send the access token through the 'Authorization' header
        },
      });
    },
    async registerEvent(conventionId, eventId) {
      const token = await this.$auth.getTokenSilently();

      const registerInfo = {
        conventionId,
        eventId,
        userId: this.$auth.user.email,
        numberOfParticipants: 1,
      };
      // Use Axios to make a call to the API
      await this.$axios.post('/api/conventions/register/event', registerInfo, {
        headers: {
          Authorization: `Bearer ${token}`, // send the access token through the 'Authorization' header
        },
      });
    },
  },
};
</script>

<style scoped>
#inlinearea {
    display:inline-block;
}
</style>
