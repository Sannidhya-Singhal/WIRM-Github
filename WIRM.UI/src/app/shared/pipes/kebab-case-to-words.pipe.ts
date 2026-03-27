import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: "kebabToWord", 
    standalone: true,
})
export class KebabToWordsPipe implements PipeTransform{
    transform(value: string) {
        return value.split('-')
                .map(word => word.charAt(0).toUpperCase() + word.slice(1))
                .join(' ');
    }
}