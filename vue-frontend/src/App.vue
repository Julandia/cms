<template>
  <div id="app">
    <header>
      <nav>
        <ul>
          <li class="nav-item">
            <router-link class="nav-link" :to="{name: 'Home'}" exact>
              Home
            </router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" :to="{name: 'About'}" exact>
              About
            </router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" :to="{name: 'Conventions'}" exact>
              Conventions
            </router-link>
          </li>
          <li class="nav-item" v-if="$auth.isAuthenticated">
            <router-link v-if="$auth.isAuthenticated" class="nav-link" :to="{name: 'MyConventions'}" exact>
              My Conventions
            </router-link>
          </li>
          <li class="nav-item" v-if="$auth.isAuthenticated">
            <router-link v-if="$auth.isAuthenticated" class="nav-link" :to="{name: 'Profile'}" exact>
              Profile
            </router-link>
          </li>
          <li class="nav-item login">
            <div v-if="!$auth.loading">
              <!-- show login when not authenticated -->
              <button v-if="!$auth.isAuthenticated" @click="login">Log in</button>
              <!-- show logout when authenticated -->
              <button v-if="$auth.isAuthenticated" @click="logout">Log out</button>
            </div>
          </li>
        </ul>
      </nav>
    </header>
    <div class="container">
      <main>
        <router-view />
      </main>
    </div>
  </div>
</template>

<script>

export default {
  name: 'MainApp',
  components: {
  },
  methods: {
    // Log the user in
    login() {
      this.$auth.loginWithRedirect();
    },
    // Log the user out
    logout() {
      this.$auth.logout({
        returnTo: window.location.origin,
      });
    },
  },
};
</script>

<style>
body {
  background: linear-gradient(to bottom, #555, #999);
  background-attachment: fixed;
}

#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
}
</style>

<style scoped>
main {
  padding: 30px;
  background-color: white;
  width: 1124px;
  min-height: 300px;
}
header {
  background-color: #999;
  width: 1184px;
  margin: 0 auto;
}
ul {
  padding: 3px;
  display: flex;
}
.nav-item {
  display: inline-block;
  padding: 5px 10px;
  font-size: 22px;
  border-right: 1px solid #bbb;
}
.nav-item.login {
  position: relative;
  margin-left: auto;
  border-right: none;
}
.logo {
  vertical-align: middle;
  height: 30px;
}
.nav-link {
  text-decoration: none;
  color: inherit;
}
.router-link-active {
  color: white;
}
.link-active {
  color: white;
}
.container {
  display: flex;
  margin: 10px auto 0 auto;
  justify-content: center;
}
</style>
