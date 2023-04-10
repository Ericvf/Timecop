class AnimationExtensions {

    Start(objectId, descriptor, delay) {
        console.log(descriptor);

        return new Promise((resolve, reject) => {
            const node = document.getElementsByClassName(objectId);

            const keyFrames = [];

            for (const keyFrame of descriptor.keyFrames) {
                var keyframeObject = {};

                for (const effect of keyFrame.effects) {
                    if (keyframeObject[effect.property]) {
                        keyframeObject[effect.property] += ' ' + effect.value;
                    }
                    else {
                        keyframeObject[effect.property] = effect.value;
                    }
                }

                keyFrames.push(keyframeObject);
            }

            if (node[0]) {
                const animation = node[0].animate(keyFrames,
                    {
                        duration: descriptor.duration,
                        easing: descriptor.easing || 'linear',
                        delay: (delay || 0) + descriptor.delayValue,
                        fill: 'both'
                    });

                animation.addEventListener('finish', () => {
                    resolve(animation.id);
                });
            } else {
                resolve('');
            }
        });
    }
}

window.AnimationExtensions = new AnimationExtensions();