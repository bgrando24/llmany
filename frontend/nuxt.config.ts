// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    compatibilityDate: "2025-05-15",
    devtools: { enabled: true },
    modules: [
      "@nuxt/eslint",
      "@nuxt/ui",
      "@nuxt/icon",
      "@nuxtjs/tailwindcss",
      "@nuxtjs/mdc",
    ],
    css: ["~/assets/css/main.css"],
    devServer: {
        https: {
            key: "./localhost-key.pem",
            cert: "./localhost.pem",
        },
    },
});