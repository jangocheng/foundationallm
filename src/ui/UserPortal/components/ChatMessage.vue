<template>
	<div
		class="message-row"
		:class="message.sender === 'User' ? 'message--out' : 'message--in'"
	>
		<div class="message">
			<div class="message__header">
				<!-- Sender -->
				<span class="header__sender">
					<img v-if="message.sender !== 'User'" class="avatar" src="~/assets/FLLM-Agent-Light.svg">
					<span>{{ getDisplayName() }}</span>
				</span>

				<!-- Tokens & Timestamp -->
				<span>
					<Chip 
						:label="`Tokens: ${message.tokens}`" 
						:class="message.sender === 'User' ? 'token-chip--out' : 'token-chip--in'" 
						:pt="{
							root: { style: { borderRadius: '24px', marginRight: '12px' } },
							label: { style: { color: message.sender === 'User' ? 'var(--primary-color)' : 'var(--accent-color)' } }
						}"
					/>
					{{ $filters.timeAgo(new Date(message.timeStamp)) }}
				</span>
			</div>

			<!-- Message text -->
			<div class="message__body">
				<template v-if="message.sender === 'Assistant' && message.type === 'LoadingMessage'">
					<i class="pi pi-spin pi-spinner"></i>
				</template>
				<span v-else>{{ displayText }}</span>
			</div>

			<div class="message__footer" v-if="message.sender !== 'User'">
				<span class="ratings">
					<!-- Like -->
					<span>
						<Button
							:disabled="message.type === 'LoadingMessage'"
							size="small"
							text
							:icon="message.rating ? 'pi pi-thumbs-up-fill' : 'pi pi-thumbs-up'"
							:label="message.rating ? 'Message Liked!' : 'Like'"
							@click.stop="handleRate(message, true)"
						/>
					</span>

					<!-- Dislike -->
					<span>
						<Button
							:disabled="message.type === 'LoadingMessage'"
							size="small"
							text
							:icon="message.rating === false ? 'pi pi-thumbs-down-fill' : 'pi pi-thumbs-down'"
							:label="message.rating === false ? 'Message Disliked.' : 'Dislike'"
							@click.stop="handleRate(message, false)"
						/>
					</span>
				</span>

				<!-- View prompt -->
				<span class="view-prompt">
					<Button
						:disabled="message.type === 'LoadingMessage'"
						size="small"
						text
						icon="pi pi-book"
						label="View Prompt"
						@click.stop="handleViewPrompt"
					/>

					<!-- Prompt dialog -->
					<Dialog
						:visible="viewPrompt"
						modal
						header="Completion Prompt"
						:closable="false"
						:style="{ width: '50vw' }"
					>
						<p class="prompt-text">{{ prompt.prompt }}</p>
						<template #footer>
							<Button label="Close" @click="viewPrompt = false" />
						</template>
					</Dialog>
				</span>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import type { Message, CompletionPrompt } from '@/js/types';
import api from '@/js/api';

export default {
	name: 'ChatMessage',

	props: {
		message: {
			type: Object as PropType<Message>,
			required: true,
		},
		showWordAnimation: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['rate'],

	data() {
		return {
			prompt: {} as CompletionPrompt,
			viewPrompt: false,
			displayText: '',
		};
	},

	created() {
		if (this.showWordAnimation) {
			this.displayWordByWord();
		} else {
			this.displayText = this.message.text;
		}
	},

	methods: {
		displayWordByWord() {
			const words = this.message.text.split(' ');
			let index = 0;

			const displayNextWord = () => {
				if (index < words.length) {
					this.displayText += words[index] + ' ';
					index++;
					setTimeout(displayNextWord, 10);
				}
			};

			displayNextWord();
		},

		getDisplayName() {
			if (this.message.senderDisplayName) {
				return this.message.sender === 'User' ? this.message.senderDisplayName : `${this.message.sender} - ${this.message.senderDisplayName}`;
			}
			return this.message.sender;
		},

		handleRate(message: Message, isLiked: boolean) {
			this.$emit('rate', { message, isLiked: message.rating === isLiked ? null : isLiked });
		},

		async handleViewPrompt() {
			const prompt = await api.getPrompt(this.message.sessionId, this.message.completionPromptId);
			this.prompt = prompt;
			this.viewPrompt = true;
		},
	},
};
</script>

<style lang="scss" scoped>
.message-row {
	display: flex;
	align-items: flex-end;
	margin-top: 8px;
	margin-bottom: 8px;
}

.message {
	padding: 12px;
	width: 80%;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
}

.message--in {
	.message {
		background-color: rgba(250, 250, 250, 1);
	}
}

.message--out {
	flex-direction: row-reverse;
	.message {
		background-color: var(--primary-color);
		color: var(--primary-text)
	}
}

.message__header {
	margin-bottom: 12px;
	display: flex;
	justify-content: space-between;
	padding-left: 12px;
	padding-right: 12px;
	padding-top: 8px;
}

.message__body {
	white-space: pre-wrap;
	overflow-wrap: break-word;
	padding-left: 12px;
	padding-right: 12px;
	padding-top: 8px;
	padding-bottom: 8px;
}

.message__footer {
	margin-top: 8px;
	display: flex;
	justify-content: space-between;
}

.header__sender {
	display: flex;
	align-items: center;
}

.avatar {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	margin-right: 12px;
}

.token-chip--out {
	background-color: var(--accent-color);
}

.token-chip--in {
	background-color: var(--primary-color);
}

.ratings {
	display: flex;
	gap: 16px;
}

.icon {
	margin-right: 4px;
	cursor: pointer;
}

.view-prompt {
	cursor: pointer;
}

.dislike {
	margin-left: 12px;
	cursor: pointer;
}

.like {
	cursor: pointer;
}

.prompt-text {
	white-space: pre-wrap;
	overflow-wrap: break-word;
}
</style>

<style lang="scss">
.p-chip .p-chip-text {
	line-height: 1.1;
	font-size: 0.75rem;
}
</style>
