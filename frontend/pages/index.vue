<template>
    <div class="max-w-3xl mx-auto h-screen">
        <div v-if="receivedResponses" class="pl-4 flex flex-col justify-center">
            <div
                v-if="receivedResponses.prompt"
                class="mb-8 flex gap-2 items-baseline"
            >
                <p class="text-lg font-bold text-primary">Your prompt:</p>
                <p class="italic font-normal">
                    {{ receivedResponses.prompt }}
                </p>
            </div>
            <div
                v-for="response in receivedResponses.responses"
                :key="index"
                class="mb-8 w-full border-2 border-neutral p-4 rounded-xl"
            >
                <h3>{{ response.serviceName }} - {{ response.modelName }}</h3>
                <p>Latency: {{ Number(response.latencyMs).toFixed(2) }} ms</p>
                <p class="mt-4">{{ response.responseText }}</p>
            </div>
        </div>
        <h2 v-else-if="!loading" class="text-center mb-16">
            Responses will display here when available
        </h2>
        <div v-else class="mb-16 flex justify-center">
            <UIcon name="svg-spinners:90-ring-with-bg" size="70" />
        </div>
        <div class="w-full max-w-2xl mx-auto mt-8">
            <UAlert
                v-if="infoAlert"
                :description="infoAlert"
                color="info"
                class="px-2 py-2 font-semibold"
            />
            <UAlert
                v-if="errorAlert"
                title="There was a problem"
                :description="errorAlert"
                color="error"
                class="px-2 py-2 font-bold"
            />
        </div>
        <form class="mt-8" @submit.prevent="handleSubmit">
            <InputTextChat
                v-model="userPrompt"
                placeholder="Enter your prompt here..."
            />
        </form>
    </div>
</template>

<script setup lang="ts">
const userPrompt = ref("");

const infoAlert = ref("");
const errorAlert = ref("");

const loading = ref(false);

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

// const receivedResponses = ref<PromptResponse | null>(null);
const receivedResponses = ref<PromptResponse>({
    prompt: "Give me a one sentence haiku about cats",
    responses: [
        {
            serviceName: "OpenAI",
            modelName: "gpt-4.1",
            responseText:
                "Soft paws in moonlight,  \nSilent whiskers drift through dreams—  \nNight’s gentle secret.",
            tokensUsed: 0,
            latencyMs: 1524.2885,
        },
        {
            serviceName: "Anthropic",
            modelName: "claude-opus-4-20250514",
            responseText: "Soft paws on moonlight.",
            tokensUsed: 0,
            latencyMs: 3793,
        },
        {
            serviceName: "DeepSeek",
            modelName: "deepseek-chat",
            responseText:
                '"Soft paws dance at dawn— / whiskers twitch in golden light— / purring sunbeam nap."',
            tokensUsed: 0,
            latencyMs: 4867,
        },
        {
            serviceName: "Google Gemini",
            modelName: "gemini-2.0-flash",
            responseText:
                "Soft paws tread lightly,\nA purring, rumbling engine,\nSunbeam nap begins.\n",
            tokensUsed: 0,
            latencyMs: 792,
        },
    ],
});

const handleSubmit = async () => {
    loading.value = true;
    // Reset previous alerts
    infoAlert.value = "";
    errorAlert.value = "";

    if (userPrompt.value.trim() === "") {
        infoAlert.value = "Please enter a prompt.";
        loading.value = false;
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
            infoAlert.value = "No responses received. Please try again.";
            return;
        }

        receivedResponses.value = response;
        userPrompt.value = ""; // Clear the input after submission
    } catch (error) {
        console.error("Error submitting prompt:", error);
        errorAlert.value =
            "An error occurred while submitting your prompt. Please try again later.";
    } finally {
        loading.value = false;
    }
};
</script>
