<template>
  <div>
    <div>
      <img :src="$auth.user.picture">
      <div v-if="$auth.isAuthenticated">
        <h2>{{ $auth.user.name }}</h2>
        <p>{{ $auth.user.email }}</p>
      </div>
      <div v-if="!$auth.isAuthenticated">
        <h2>Not authenticated</h2>
      </div>
    </div>

    <div>
      <pre>{{ JSON.stringify($auth.user, null, 2) }}</pre>
    </div>
  </div>
</template>
