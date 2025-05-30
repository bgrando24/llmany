<template>
    <div
        :class="props.customStyling"
        class="w-full flex flex-col rounded-3xl border-2 border-primary border-primary"
    >
        <textarea
            ref="textareaRef"
            v-model="model"
            @input="resizeTextarea"
            rows="1"
            class="w-full resize-none overflow-y-auto rounded-md p-4 focus:outline-none transition"
            placeholder="Enter your prompt..."
        />
        <div class="flex justify-end pr-3 pb-3">
            <button
                class="z-10 bg-primary rounded-full w-12 h-12 flex items-center justify-center hover:opacity-90 hover:cursor-pointer"
            >
                <UIcon
                    class="w-8 h-8 text-white dark:fill-black"
                    name="pixelarticons:arrow-bar-up"
                />
            </button>
        </div>
    </div>
</template>

<script setup lang="ts">
const model = defineModel<string>();
const props = defineProps<{
    customStyling?: string;
    placeholder?: string;
    maxRows?: number;
}>();

const textareaRef = ref<HTMLTextAreaElement>();
const maxHeight = 256; // px, roughly ~8 rows depending on font (32px per row)

const resizeTextarea = () => {
    if (!textareaRef.value) return;

    textareaRef.value.style.height = "auto"; // Reset to shrink if needed
    textareaRef.value.style.overflowY = "hidden";

    if (textareaRef.value.scrollHeight > maxHeight) {
        textareaRef.value.style.height = `${maxHeight}px`;
        textareaRef.value.style.overflowY = "auto";
    } else {
        textareaRef.value.style.height = `${textareaRef.value.scrollHeight}px`;
    }
};

onMounted(() => {
    resizeTextarea();
});
</script>

<style scoped>
textarea {
    scrollbar-color: var(--color-primary) transparent;
    scrollbar-width: auto;
}
</style>
