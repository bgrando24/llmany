<template>
    <div>
        <NavTopBar />
        <slot />
    </div>
</template>

<script setup lang="ts">
const colourMode = useColorMode();
colourMode.preference = "light";
</script>
