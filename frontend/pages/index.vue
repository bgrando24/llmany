<template>
    <div class="max-w-2xl mx-auto">
        <h1 class="text-green-500">LLMany</h1>
        <form @submit.prevent="handleSubmit">
            <UInput
                v-model="userPrompt"
                class="w-full"
                placeholder="Type your prompt here..."
            />
            <UButton class="" type="submit" label="Send" />
        </form>
        <div v-if="receivedResponses">
            <div
                v-for="(response, index) in receivedResponses.responses"
                :key="index"
                class="mb-4"
            >
                <h3>{{ response.serviceName }} - {{ response.modelName }}</h3>
                <p>{{ response.responseText }}</p>
                <p>Latency: {{ response.latencyMs }}</p>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
const userPrompt = ref("");

// "prompt": "Give me a one sentence haiku about cats",
//     "responses": [
//         {
//             "serviceName": "OpenAI",
//             "modelName": "gpt-4.1",
//             "responseText": "Soft paws in moonlight,  \nSilent whiskers drift through dreams—  \nNight’s gentle secret.",
//             "tokensUsed": 0,
//             "latencyMs": 1524.2885
//         },
interface PromptResponse {
    prompt: string;
    responses: {
        serviceName: string;
        modelName: string;
        responseText: string;
        tokensUsed: number;
        latencyMs: number;
    }[];
}

const receivedResponses = ref<PromptResponse | null>(null);

const handleSubmit = async () => {
    if (userPrompt.value.trim() === "") {
        alert("Please enter a prompt.");
        return;
    }

    try {
        const response = (await $fetch("https://localhost:7185/Prompt", {
            method: "POST",
            body: {
                prompt: userPrompt.value,
            },
        })) as PromptResponse;

        if (
            !response ||
            !response.responses ||
            response.responses.length === 0
        ) {
            alert("No responses received.");
            return;
        }

        receivedResponses.value = response;
        userPrompt.value = ""; // Clear the input after submission
    } catch (error) {
        console.error("Error submitting prompt:", error);
        alert(
            "An error occurred while submitting your prompt. Please try again."
        );
    }
};
</script>
