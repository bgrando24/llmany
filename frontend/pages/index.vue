<template>
    <div class="max-w-3xl mx-auto">
        <div
            v-if="receivedResponses"
            class="flex flex-col justify-center mb-36"
        >
            <div v-if="receivedResponses.prompt" class="mb-8">
                <div
                    class="w-full bg-neutral-200 dark:bg-neutral-800 px-2 py-4 rounded-xl"
                >
                    <div class="flex gap-2 items-baseline">
                        <p
                            class="text-lg font-bold text-primary whitespace-nowrap"
                        >
                            Your prompt:
                        </p>
                        <p class="italic font-normal">
                            {{ receivedResponses.prompt }}
                        </p>
                    </div>
                </div>
                <UModal transition>
                    <Btn
                        class="bg-secondary mx-auto"
                        @click="handleGetComparison"
                        >Compare</Btn
                    >
                    <template #body>
                        <div
                            v-if="comparisonLoading"
                            class="mb-16 flex justify-center"
                        >
                            <UIcon
                                name="svg-spinners:90-ring-with-bg"
                                size="70"
                            />
                        </div>
                        <div v-else>
                            <p
                                v-html="
                                    marked.parse(
                                        comparisonResponse.responseText
                                    )
                                "
                                class="text-sm wrap-break-word"
                                style="
                                    white-space: pre-wrap;
                                    word-break: break-word;
                                "
                            ></p>
                        </div>
                    </template>
                </UModal>
            </div>
            <div
                v-for="response in receivedResponses.responses"
                :key="index"
                class="mb-8 w-full border-2 border-neutral p-4 rounded-xl"
            >
                <h3>{{ response.serviceName }} - {{ response.modelName }}</h3>
                <p>Latency: {{ Number(response.latencyMs).toFixed(2) }} ms</p>
                <div
                    class="mt-4"
                    v-html="marked.parse(response.responseText)"
                ></div>
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
const comparisonLoading = ref(false);

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

interface ComparisonResponse {
    serviceName: string;
    modelName: string;
    responseText: string;
    tokensUsed: number;
    latencyMs: number;
}

const receivedResponses = ref<PromptResponse | null>(null);
const comparisonResponse = ref<ComparisonResponse | null>(null);
// const comparisonResponse = ref<ComparisonResponse | null>({
//     serviceName: "OpenAI",
//     modelName: "o4-mini",
//     responseText:
//         "# Is a Groodle Right for a Young Couple with Resident Cats?\n\nA Groodle (Golden Retriever × Poodle) can be an excellent companion for a young couple—cats included—so long as you’re prepared for the key commitments below.\n\n## Pros  \n- **Affectionate & Intelligent**  \nEager to please, quick learners and generally happy to snuggle.  \n- **Pet-Friendly (with Prep)**  \nGolden-inspired sociability + Poodle smarts means they can cohabit peacefully if you invest in early, positive cat introductions.  \n- **Moderate–Low Shedding**  \n  Their curly coats shed less than many double-coats (note: not guaranteed hypoallergenic).  \n- **Versatile Energy**  \n  Loves walks, fetch and puzzle toys—ideal if you enjoy active play or dog sports.\n\n## Cons  \n- **Grooming Demands**  \n  Professional trims every 6–8 weeks plus daily brushing to prevent mats.  \n- **High Stimulation Needs**  \n  Plan on 1–2 hours per day of walks, play or training sessions to avert boredom and destructive behaviors.  \n- **Prey Drive Potential**  \n  Without proper socialization, a Groodle may view cats as “chase toys.”  \n- **Separation Anxiety Risk**  \n  They thrive on company—days alone can lead to stress-driven chewing or barking.\n\n## Key Recommendations  \n1. **Gradual Cat Introductions**  \n   – Use scent swaps, controlled on-leash meetings and reward calm interactions.  \n2. **Positive-Reinforcement Training**  \n   – Leverage their smarts with treats, clicker work and short, frequent sessions.  \n3. **Daily Enrichment**  \n   – Combine physical exercise with mind-benders (Kong puzzles, obedience drills, scent games).  \n4. **Consistent Routine**  \n   – Regular feeding, walking and grooming schedules help prevent anxiety.  \n5. **Choose Carefully**  \n   – Meet the puppy’s parents (especially the Poodle side) to gauge energy levels and temperament.\n\n## Bottom Line  \nIf you can commit to structured training, focused socialization with your cats, daily exercise and a solid grooming regimen, a Groodle will likely become a loving, adaptable—and low-shedding—member of your multi-pet household.",
//     tokensUsed: 0,
//     latencyMs: 6799.0818,
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

const handleGetComparison = async () => {
    comparisonLoading.value = true;
    if (!receivedResponses.value) {
        infoAlert.value = "No responses available for comparison.";
        return;
    }

    if (comparisonResponse.value) return;

    try {
        const response = (await $fetch(
            "https://localhost:7185/Prompt/compare-latest",
            {
                method: "GET",
            }
        )) as ComparisonResponse;

        if (!response || !response.responseText) {
            infoAlert.value = "No comparison response received.";
            return;
        }

        comparisonResponse.value = response;
        comparisonLoading.value = false;
    } catch (error) {
        console.error("Error getting comparison:", error);
        errorAlert.value =
            "An error occurred while getting the comparison. Please try again later.";
        comparisonLoading.value = false;
    } finally {
        comparisonLoading.value = false;
    }
};
</script>
