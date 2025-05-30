<template>
    <nav
        class="z-50 bg-neutral-50 dark:bg-neutral-900 top-0 sticky shadow-md mb-16"
    >
        <div class="container h-16 flex justify-between items-center">
            <h1>
                <NuxtLink to="/">LLMany</NuxtLink>
            </h1>
            <div class="flex items-center space-x-2">
                <USwitch
                    v-model="useDarkMode"
                    size="xl"
                    checked-icon="pixelarticons:moon-star"
                    unchecked-icon="pixelarticons:lightbulb-on"
                />
            </div>
        </div>
    </nav>
</template>

<script setup lang="ts">
const colourMode = useColorMode();
const useDarkMode = ref(true);

watch(useDarkMode, (newValue) => {
    colourMode.preference = newValue ? "dark" : "light";
});
</script>
