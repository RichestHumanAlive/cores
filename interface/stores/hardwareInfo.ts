import { writable, get } from "svelte/store"

const defaultHardwareInfo: HardwareInfo = {
	CPU: {
		name: "CPUName",
		lastLoad: 15,
		temperature: [
			{
				value: 50,
				min: 40,
				max: 60,
			},
			{
				value: 50,
				min: 40,
				max: 60,
			},
		],
	},

	GPU: {
		name: "CPUName",
		lastLoad: 15,
		temperature: [
			{
				value: 50,
				min: 40,
				max: 60,
			},
		],
	},

	RAM: {
		load: [],
	},
}

// export const hardwareInfo = writable<HardwareInfo>(defaultHardwareInfo)
export const hardwareInfo = writable<HardwareInfo>(sessionStorage.hardwareInfo ? JSON.parse(sessionStorage.hardwareInfo) : defaultHardwareInfo)

hardwareInfo.subscribe((data) => {
	console.log("HardwareInfo changed: ", data)

	sessionStorage.setItem("hardwareInfo", JSON.stringify(data))
})

export const getHardwareInfo = (): HardwareInfo => {
	return get(hardwareInfo)
}

export const setHardwareInfo = (newState: HardwareInfo) => {
	hardwareInfo.set(newState)
}
