import { Pipe, PipeTransform } from '@angular/core';
import * as _ from 'lodash/fp';
@Pipe({
  name: 'searchFilter',

})
export class SearchFilterPipe implements PipeTransform {

  // transform(value: any, searchText: any): any {
  //   if(!searchText) {
  //     return value;
  //   }
  //   return value.filter((data: any) => this.matchValue(data,searchText));
  // }

  // matchValue(data:any, value:any) {
  //   return Object.keys(data).map((key) => {
  //      return new RegExp(value, 'gi').test(data[key]);
  //   }).some(result => result);
  // }

  // transform(users: any[], args: any): any {
  //   return users.reduce((coll, top) => {

  //     if (top.subItemsList.length > 0) {
  //       return coll.concat(top.subItemsList.reduce((subColl:any, sub:any) => {
  //         if (sub.items.length > 0) {
  //           return subColl.concat(sub.items);
  //         }
  //         return subColl;
  //       }, []));
  //     }
  //     return coll;
  //   }, []).filter((user: { itemName: string; }) => user.itemName.toLowerCase().includes(args.toLowerCase()));
  // }


  transform(items: Array<any>, filter: { [key: string]: any }): Array<any> {
    return items.filter(item => {
      return this.findItem(item, filter);
    });
  }

  findItem(currentItem: any, filterItem: any): boolean {
    let keysInFilter = Object.keys(filterItem).find(filterKey => {
      let isItemFound = false;
      if (filterKey === 'data') {
        isItemFound = currentItem?.data.find((orgItems: any) => orgItems[filterKey] === filterItem[filterKey]) !== null;
      } else {
        isItemFound = currentItem[filterKey] === filterItem[filterKey]
      }

      return isItemFound;
    })

    return Object.keys(filterItem).length === keysInFilter?.length;
  }


  // transform(items: any[],searchTerm:any){
  //   if(searchTerm){let newSearchTerm=!isNaN(searchTerm)? searchTerm.toString(): searchTerm.toString().toUpperCase();return items.filter(item=>{
  //   return this.lookForNestedObject(item,newSearchTerm);
  //   })
  //   }
  //   else{return items;}
  //   }
  //   lookForNestedObject(item:any,newSearchTerm:any){
  //   let origItem={...item};
  //   let that=this;
  //   let count=0;
  //   parseNestedObject(item);
  //   function parseNestedObject(item:any){
  //   for(let key in item){
  //   if(_.isPlainObject(item[key])){
  //   if(origItem[key]) { delete origItem[key]}
  //   parseNestedObject(item[key]);
  //   }
  //   else if(Array.isArray(item[key])){
  //   if(origItem[key]) { delete origItem[key]}
  //   parseNestedObject(item[key]);
  //   }
  //   else{
  //   count++;
  //   if(origItem[key]) { delete origItem[key]}
  //   origItem[key+count]=item[key];
  //   }}
  //   }
  //   return that.search(item,origItem,newSearchTerm);
  //   }



  //   search(item:any,origItem:any,newSearchTerm:any){
  //   let filteredList=[];
  //   let prop="";
  //   for(let koy in origItem){
  //     prop=isNaN(origItem[koy]) ? origItem[koy].toString().toUpperCase() : origItem[koy].toString();
  //     if(prop.indexOf(newSearchTerm) > -1){
  //   filteredList.push(item);

  //   }

  // }
  // return filteredList;
  //   }


}
