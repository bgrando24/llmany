// @ts-check
import withNuxt from "./.nuxt/eslint.config.mjs";

export default withNuxt({
    rules: {
        "vue/vue/html-self-closing": [
            "error",
            {
                input: "never",
            },
        ],
    },
});
