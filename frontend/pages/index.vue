<template>
    <div class="max-w-3xl mx-auto">
        <div
            v-if="receivedResponses"
            class="flex flex-col justify-center mb-36"
        >
            <div
                v-if="receivedResponses.prompt"
                class="mb-8 w-full bg-neutral-200 dark:bg-neutral-800 px-2 py-4 rounded-xl flex gap-2 items-baseline"
            >
                <p class="text-lg font-bold text-primary whitespace-nowrap">
                    Your prompt:
                </p>
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
                <p
                    class="mt-4"
                    v-html="marked.parse(response.responseText)"
                ></p>
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
        <form
            class="mt-8 w-full max-w-3xl rounded-t-3xl bg-neutral-50 dark:bg-neutral-900 fixed bottom-0 pb-4"
            @submit.prevent="handleSubmit"
        >
            <InputTextChat
                v-model="userPrompt"
                placeholder="Enter your prompt here..."
            />
        </form>
    </div>
</template>

<script setup lang="ts">
import { marked } from "marked";

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

const receivedResponses = ref<PromptResponse | null>(null);
// const receivedResponses = ref<PromptResponse | null>({
//     prompt: "Give me a short sentence haiku about cats",
//     responses: [
//         {
//             serviceName: "OpenAI",
//             modelName: "o4-mini",
//             responseText:
//                 "Soft paws tread moonlight, whiskers twitch in silent hunt, dawn greets purring warmth.",
//             tokensUsed: 0,
//             latencyMs: 4217.3648,
//         },
//         {
//             serviceName: "Anthropic",
//             modelName: "claude-3-5-haiku-latest",
//             responseText:
//                 "Here's a haiku about cats:\n\n```\nSoft paws, whiskers twitch\nMoonlight gleams on silent fur\nPurring, dreams unfold\n```",
//             tokensUsed: 0,
//             latencyMs: 1758,
//         },
//         {
//             serviceName: "DeepSeek",
//             modelName: "deepseek-chat",
//             responseText:
//                 "**Paws tap soft moonlight,**  \n**whiskers twitch with dreams so sweet—**  \n**night belongs to cats.**  \n\n(5-7-5 syllables)",
//             tokensUsed: 0,
//             latencyMs: 8086,
//         },
//         {
//             serviceName: "Google Gemini",
//             modelName: "gemini-2.0-flash",
//             responseText:
//                 "Soft paws tread so light,\nA purring rumble vibrates,\nSunbeam nap unfolds.\n",
//             tokensUsed: 0,
//             latencyMs: 1637,
//         },
//         {
//             serviceName: "Google Gemini",
//             modelName: "gemini-2.0-flash",
//             responseText:
//                 "Soft paws tread so light,\nA purring rumble vibrates,\nSunbeam nap unfolds.\n",
//             tokensUsed: 0,
//             latencyMs: 1637,
//         },
//         {
//             serviceName: "Google Gemini",
//             modelName: "gemini-2.0-flash",
//             responseText:
//                 "Soft paws tread so light,\nA purring rumble vibrates,\nSunbeam nap unfolds.\n",
//             tokensUsed: 0,
//             latencyMs: 1637,
//         },
//         {
//             serviceName: "Google Gemini",
//             modelName: "gemini-2.0-flash",
//             responseText:
//                 "Soft paws tread so light,\nA purring rumble vibrates,\nSunbeam nap unfolds.\n",
//             tokensUsed: 0,
//             latencyMs: 1637,
//         },
//     ],
// });

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
